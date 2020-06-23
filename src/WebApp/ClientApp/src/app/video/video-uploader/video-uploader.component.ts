import { Component, OnInit } from '@angular/core';
import {VideoUploadService} from './video-upload.service'

@Component({
  selector: 'app-video-uploader',
  templateUrl: './video-uploader.component.html',
  styleUrls: ['./video-uploader.component.scss']
})
export class VideoUploaderComponent implements OnInit {

  constructor(
    public videoUploadService: VideoUploadService
  ) { }

  ngOnInit(): void {
  }

  onFileSelected($event: Event) {
    const target = $event.target as HTMLInputElement
    const files = Array.from(target.files)
    target.value = ''

    this.videoUploadService.addFiles(files)
  }
}
