import {AfterViewInit, Component, ElementRef, Input, OnInit, ViewChild} from '@angular/core'
declare const shaka: any

@Component({
  selector: 'app-shaka-player',
  templateUrl: './shaka-player.component.html',
  styleUrls: ['./shaka-player.component.scss']
})
export class ShakaPlayerComponent implements OnInit, AfterViewInit {
  @Input() manifestPath: string
  @Input() accessToken: string
  @ViewChild('videoElement') videoElement: ElementRef
  @ViewChild('videoContainer') videoContainer: ElementRef
  private player

  constructor() { }

  ngOnInit(): void {

  }

  ngAfterViewInit() {
    const videoElm = this.videoElement.nativeElement
    const player = new shaka.Player(videoElm)
    const ui = new shaka.ui.Overlay(player, this.videoContainer.nativeElement, videoElm);
    console.log(ui)

    player.getNetworkingEngine().registerRequestFilter((type, request) => {
      if (type == shaka.net.NetworkingEngine.RequestType.LICENSE && this.accessToken) {
        request.headers['Authorization'] = `Bearer ${this.accessToken}`
      }
    })

    this.player = player
    if (this.manifestPath) {
      this.player.load(this.manifestPath)
    }
  }

}
