import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import {VideoComponent} from './video.component'
import {VideoListComponent} from './video-list/video-list.component'
import {AssetPlayerComponent} from './asset-player/asset-player.component'


const routes: Routes = [
  {
    path: '',
    component: VideoComponent,
    children: [{
      path: '',
      component: VideoListComponent
    }, {
      path: 'play/:assetName',
      component: AssetPlayerComponent
    }]
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class VideoRoutingModule { }
