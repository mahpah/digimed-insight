import {AfterViewInit, Component, ElementRef, Input, OnInit, ViewChild} from '@angular/core'
declare const amp: any

@Component({
  selector: 'app-azure-media-player',
  templateUrl: './azure-media-player.component.html',
  styleUrls: ['./azure-media-player.component.scss']
})
export class AzureMediaPlayerComponent implements OnInit, AfterViewInit {
  @ViewChild('videoElement') private videoElement: ElementRef
  @Input() manifestPath: string
  @Input() accessToken: string

  constructor() { }

  ngOnInit(): void {
  }

  ngAfterViewInit() {
    const player = amp(this.videoElement.nativeElement, {
      nativeControlsForTouch: false,
      autoplay: false,
      controls: true,
      width: "640",
      height: "400",
      poster: ""
    }, function () {
      console.log('Player created', this)
    })

    player.src([{
      src: this.manifestPath,
      protectionInfo: [{
        type: 'Widevine',
        authenticationToken: `Bearer=${this.accessToken}`
      }, {
        type: 'PlayReady',
        authenticationToken: `Bearer=${this.accessToken}`
      }, {
        type: 'AES',
        authenticationToken: `Bearer=${this.accessToken}`
      }]
    }])
  }

}
