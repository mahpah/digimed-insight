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
    this.baseUrl = `${baseUrl}api/video/`
  }

  generateUploadUrl(fileName: string, contentType: string) {
    return this.httpClient.post<UploadKey>(`${this.baseUrl}upload`, {
      fileName,
      contentType
    })
  }

  putFile(file: File, url: string) {
    const form = new FormData()
    form.append('file', file)
    const uploadUrl = url.replace('?', `/${file.name}?`)
    return this.httpClient.put(uploadUrl, file, {
      observe: 'events',
      reportProgress: true,
      headers: {
        'x-ms-blob-type': 'BlockBlob',
      }
    })
  }

  cleanAssets(assetNames: string[]) {
    return this.httpClient.post(`${this.baseUrl}cleanUp`, {
      assetNames
    })
  }

  listJobs(nextToken: string = '') {
    return this.httpClient.get<Page<JobInformation>>(`${this.baseUrl}jobs`, {
      params: {
        nextToken
      }
    })
  }

  process(inputAssetName: string) {
    return this.httpClient.post(`${this.baseUrl}process`, {
      inputAssetName
    })
  }

  getStreamingInformation(assetName: string, protectionType: 'drm' | 'aes') {
    return this.httpClient.get<{
      manifestPath: string
      accessToken: string
    }>(`${this.baseUrl}streamingInformation/${protectionType}`, {
      params: {
        assetName
      }
    })
  }
}

export interface UploadKey {
  assetName: string
  uploadUrl: string
}

export interface Page<T> {
  items: T[],
  nextToken: string
}

export interface JobInformation {
  name: string
  state: string
  elapsedTime: string
  output: string
  input: string
  createdDate: Date
  file: string
}
