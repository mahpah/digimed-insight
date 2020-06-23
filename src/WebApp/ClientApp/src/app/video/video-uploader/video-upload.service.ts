import {EventEmitter, Injectable} from '@angular/core'
import {VideoApiService} from '../../../api/video-api.service'
import {v4} from 'uuid'
import {of, Subject, Subscription} from 'rxjs'
import {catchError, filter, map, mergeMap, switchMap, tap} from 'rxjs/operators'
import {HttpEvent, HttpEventType} from '@angular/common/http'

@Injectable({
  providedIn: 'root'
})
export class VideoUploadService {
  private files = new Subject<File>()
  public queue: FileUploadModel[] = []
  private subscription: Subscription
  events = new EventEmitter()

  constructor(
    private videoApi: VideoApiService
  ) {
    this.start()
  }

  addFiles(files: File[]) {
    files.forEach(x => this.files.next(x))
  }

  abort() {
    this.subscription.unsubscribe()
    this.queue
      .filter(x => x.status !== 'success' && x.status !== 'error')
      .forEach(x => {
        x.status = 'cancelled'
        x.progress = undefined
      })

    const cancelledItems = this.queue.filter(x => x.status === 'cancelled' && x.assetName)
    this.videoApi.cleanAssets(cancelledItems.map(x => x.assetName))
      .subscribe(() => {
        cancelledItems.forEach(x => x.assetName = undefined)
      })

    this.start()
  }

  private start() {
    this.subscription = this.files.pipe(
      map(file => new FileUploadModel(file)),
      tap(x => {
        this.queue.push(x)
      }),
      mergeMap(this.upload, 2),
    ).subscribe()
  }

  private upload = (model: FileUploadModel) => {
    model.status = 'init'
    return this.videoApi
      .generateUploadUrl(model.file.name, model.file.type)
      .pipe(
        tap((config) => {
          model.status = 'uploading'
          model.assetName = config.assetName
        }),
        switchMap(config => this.videoApi.putFile(model.file, config.uploadUrl)),
        tap((ev: HttpEvent<any>) => {
          if (ev.type === HttpEventType.UploadProgress) {
            model.progress = ev.loaded / ev.total
            model.status = 'uploading'
            return
          }

          if (ev.type === HttpEventType.Response) {
            model.status = 'finalizing'
            return
          }
        }),
        filter(x => x.type === HttpEventType.Response),
        switchMap(x => this.videoApi.process(model.assetName)),
        tap(() => {
          this.events.emit({
            type: 'uploadCompleted',
            file: model.file.name
          })
          model.status = 'success'
        }),
        catchError(err => {
          model.status = 'error'
          model.errorMessage = err ? err.detail : 'Error occured'
          return of('Error')
        }))
  }

  clear() {
    this.queue = this.queue.filter(x => x.status !== 'cancelled' && x.status !== 'error' && x.status !== 'success')
  }
}

export type FileUploadStatus = 'pending' | 'init' | 'uploading' | 'success' | 'error' | 'cancelled' | 'finalizing'

export class FileUploadModel {
  id: string
  status: FileUploadStatus
  progress: number
  errorMessage: string
  assetName: string

  constructor(public file: File) {
    this.id = v4()
    this.status = 'pending'
  }
}
