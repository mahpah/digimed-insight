import {Inject, Injectable} from '@angular/core'
import {HttpClient} from '@angular/common/http'

@Injectable({
  providedIn: 'root'
})
export class VideoApiService {
  private readonly baseUrl: string

  constructor(
    @Inject('BASE_URL') baseUrl: string,
    private httpClient: HttpClient
  ) {
    this.baseUrl = `${baseUrl}/api/video/`
  }

  generateUploadUrl(fileName: string) {
    return this.httpClient.post<UploadKey>(`${this.baseUrl}upload`, {
      fileName
    })
  }
}

export interface UploadKey {
  assetName: string
  uploadUrl: string
}
