import {Component, ElementRef, OnInit, ViewChild, ViewChildren} from '@angular/core'
import {VideoApiService} from '../../../api/video-api.service'
import {ActivatedRoute} from '@angular/router'
import {switchMap, tap} from 'rxjs/operators'
declare const shaka: any

@Component({
  selector: 'app-asset-player',
  templateUrl: './asset-player.component.html',
  styleUrls: ['./asset-player.component.scss']
})
export class AssetPlayerComponent implements OnInit {
  drmManifestPath: string
  aesManifestPath: string
  accessToken: string
  isShakaSupported: boolean
  isLoading: boolean
  aesAccessToken: string

  constructor(
    private videoApiService: VideoApiService,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.route.params
      .pipe(
        tap(() => this.isLoading = true),
        switchMap(p => this.videoApiService.getStreamingInformation(p.assetName, 'drm')))
      .subscribe(res => {
        this.drmManifestPath = res.manifestPath
        this.accessToken = res.accessToken
        this.isLoading = false
        this.initPlayer()
      })
    this.route.params
      .pipe(
        switchMap(p => this.videoApiService.getStreamingInformation(p.assetName, 'aes')))
      .subscribe(res => {
        this.aesManifestPath = res.manifestPath
        this.aesAccessToken = res.accessToken
      })
  }

  private initPlayer() {
    shaka.polyfill.installAll();
    this.isShakaSupported = !!shaka.Player.isBrowserSupported();
  }
}
