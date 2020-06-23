import { Component, OnInit } from '@angular/core';
import {VideoApiService, JobInformation} from '../../../api/video-api.service'
import {ActivatedRoute} from '@angular/router'
import {filter, switchMap, tap} from 'rxjs/operators'
import {Toastr} from '../../../shared/toastr.service'
import {VideoUploadService} from '../video-uploader/video-upload.service'
import {merge} from 'rxjs'

@Component({
  selector: 'app-video-list',
  templateUrl: './video-list.component.html',
  styleUrls: ['./video-list.component.scss']
})
export class VideoListComponent implements OnInit {
  videos: JobInformation[]
  isLoading: boolean
  private nextToken: string

  constructor(
    private videoApiService: VideoApiService,
    private route: ActivatedRoute,
    private toastr: Toastr,
    private videoUploadService: VideoUploadService
  ) { }

  ngOnInit(): void {
    const uploaded = this.videoUploadService.events.pipe(filter(x => x.type === 'uploadCompleted'))
    merge(this.route.params, uploaded)
      .pipe(
        tap(() => this.isLoading = true),
        switchMap(params => {
          const {nextToken} = params
          return this.videoApiService.listJobs(nextToken)
        })
      ).subscribe(res => {
        this.isLoading = false
        this.videos = res.items
        this.nextToken = res.nextToken
      }, err => {
        this.toastr.error('List video failed')
        this.isLoading = false
      })
  }

}
