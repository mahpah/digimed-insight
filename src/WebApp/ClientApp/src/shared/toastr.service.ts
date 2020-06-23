import {Injectable} from '@angular/core'
import AWN from "awesome-notifications"

@Injectable({
  providedIn: 'root'
})
export class Toastr{
  private notifier: any
  constructor() {
    this.notifier = new AWN({
      position: 'bottom-left'
    })
  }

  success(message, options = null) {
    this.notifier.success(message, options)
  }

  error(message, fallbackMesage: string = null) {
    if (typeof message === 'string') {
      this.notifier.alert(message)
      return
    }

    if (message && message.hasOwnProperty('detail')) {
      this.notifier.alert(message.detail)
      return
    }

    this.notifier.alert(fallbackMesage)
  }

  modal(message: string, options = null) {
    this.notifier.modal(`<div>${message}</div>`, options)
  }

  info(message: string, options = null) {
    this.notifier.info(message, options)
  }
}
