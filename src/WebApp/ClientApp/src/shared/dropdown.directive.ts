import {Directive, ElementRef, EventEmitter, Host, HostBinding, HostListener, OnDestroy, OnInit} from '@angular/core'
import {BehaviorSubject, Subscription} from 'rxjs'

@Directive({
  selector: '.dropdown'
})
export class DropdownDirective {
  @HostBinding('class.show') isOpen
  changed = new BehaviorSubject(false)

  private set _isOpen(x: boolean) {
    this.changed.next(x)
    this.isOpen = x
  }

  constructor(
    private elmRef: ElementRef
  ) {}

  @HostListener('document:click', ['$event'])
  onClick(event: MouseEvent) {
    this._isOpen = false
  }

  toggle() {
    this._isOpen = !this.isOpen
  }
}


@Directive({
  selector: '.dropdown-toggle'
})
export class DropdownToggleDirective implements OnInit, OnDestroy {
  @HostBinding('class.show') isOpen: boolean
  private sub: Subscription

  constructor(@Host() private host: DropdownDirective) {

  }

  ngOnInit() {
    this.sub = this.host.changed.subscribe(x => {
      this.isOpen = x
    })
  }

  ngOnDestroy() {
    this.sub.unsubscribe()
  }

  @HostListener('click', ['$event'])
  onClick(event: MouseEvent) {
    event.stopPropagation()
    this.host.toggle()
  }
}

@Directive({
  selector: '.dropdown-menu',
})
export class DropdownMenuDirective implements OnInit, OnDestroy  {
  private sub: Subscription
  @HostBinding('class.show') isOpen: boolean

  constructor(@Host() private host: DropdownDirective) {

  }

  ngOnInit() {
    this.sub = this.host.changed.subscribe(x => {
      this.isOpen = x
    })
  }

  ngOnDestroy() {
    this.sub.unsubscribe()
  }
}
