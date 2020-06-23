import { Component, OnInit } from '@angular/core';
import {VideoApiService} from '../../api/video-api.service'

@Component({
  selector: 'app-video',
  templateUrl: './video.component.html',
  styleUrls: ['./video.component.css']
})
export class VideoComponent implements OnInit {

  constructor(
    private videoApiService: VideoApiService
  ) { }

  ngOnInit(): void {
  }

}
