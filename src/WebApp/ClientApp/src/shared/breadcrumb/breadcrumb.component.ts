import {Component, EventEmitter, Injectable, OnInit} from '@angular/core'
import { v4 } from 'uuid'

@Injectable({
  providedIn: 'root'
})
export class BreadcrumbTrackerService {
  private items: Record<number, Breadcrumb>
  private ids: string[] = []
  changed = new EventEmitter<Breadcrumb[]>()

  constructor() {
    this.items = []
  }

  update(id: string, data: Breadcrumb) {
    this.items[id] = data
    const items = this.ids.map(id => this.items[id])
    this.changed.emit(items)
  }

  add() {
    const newId = v4()
    this.ids = [
      ...this.ids,
      newId
    ]
    return newId
  }

  remove(id: string) {
    this.ids = this.ids.filter(x => x !== id)
    this.items[id] = undefined
    const items = this.ids.map(id => this.items[id])
    this.changed.emit(items)
  }
}

@Component({
  selector: 'app-breadcrumb',
  templateUrl: './breadcrumb.component.html',
  styleUrls: ['./breadcrumb.component.scss']
})
export class BreadcrumbComponent implements OnInit {
  items: Breadcrumb[]

  constructor(
    private breadcrumbTracker: BreadcrumbTrackerService
  ) { }

  ngOnInit() {
    this.breadcrumbTracker.changed.subscribe(x => this.items = x)
  }
}

export interface Breadcrumb {
  path: string[] | string,
  label: string
}
