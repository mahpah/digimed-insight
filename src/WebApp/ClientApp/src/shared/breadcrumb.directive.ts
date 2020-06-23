import {Directive, Input, OnChanges, OnDestroy, OnInit} from '@angular/core'
import {Router, NavigationEnd} from '@angular/router'
import {filter, tap} from 'rxjs/operators'
import {BreadcrumbTrackerService} from './breadcrumb/breadcrumb.component'

@Directive({
  selector: '[appBreadcrumb]'
})
export class BreadcrumbDirective implements OnInit, OnChanges, OnDestroy{
  @Input() label: string
  @Input() path: string[] | string
  private id: string

  constructor(
    private router: Router,
    private breadcrumbTrackerService: BreadcrumbTrackerService
  ) {
  }

  ngOnInit() {
    if (!this.id) {
      this.id = this.breadcrumbTrackerService.add()
    }

    this.breadcrumbTrackerService.update(this.id, {
      label: this.label,
      path: this.path
    })
  }

  ngOnChanges() {
    if (this.id) {
      this.breadcrumbTrackerService.update(this.id, {
        label: this.label,
        path: this.path
      })
    }
  }

  ngOnDestroy(): void {
    this.breadcrumbTrackerService.remove(this.id)
  }

}
