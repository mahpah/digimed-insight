import {NgModule} from '@angular/core'
import {CommonModule} from '@angular/common'

import {VideoRoutingModule} from './video-routing.module'
import {VideoComponent} from './video.component'
import {VideoUploaderComponent} from './video-uploader/video-uploader.component'
import {SharedModule} from '../../shared/shared.module'
import {VideoListComponent} from './video-list/video-list.component'
import {AssetPlayerComponent} from './asset-player/asset-player.component'
import {ShakaPlayerComponent} from './shaka-player/shaka-player.component'
import {AzureMediaPlayerComponent} from './azure-media-player/azure-media-player.component'


@NgModule({
  declarations: [
    VideoComponent,
    VideoUploaderComponent,
    VideoListComponent,
    AssetPlayerComponent,
    ShakaPlayerComponent,
    AzureMediaPlayerComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    VideoRoutingModule,
  ],
})
export class VideoModule {
}
