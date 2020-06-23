import {
  AfterContentInit,
  ContentChildren,
  Directive, ElementRef,
  Optional,
  QueryList, Renderer2,
} from '@angular/core'
import {FormControlName, FormGroupDirective} from '@angular/forms'
import {BehaviorSubject, combineLatest, merge} from 'rxjs'
import {filter, mapTo, startWith} from 'rxjs/operators'

@Directive({
  selector: '.form-group'
})
export class FormGroupClassDirective implements AfterContentInit {
  @ContentChildren(FormControlName) formControlName : QueryList<FormControlName>
  private hasErrors = new BehaviorSubject(false)

  constructor(
    @Optional() private formGroupDirective: FormGroupDirective,
    private renderder: Renderer2,
    private elmRef: ElementRef
  ) { }

  ngAfterContentInit() {
    if (!this.formGroupDirective) {
      return
    }

    combineLatest([
      this.formGroupDirective.valueChanges.pipe(mapTo(true), startWith(true)),
      this.formGroupDirective.ngSubmit.pipe(mapTo(true))
    ]).pipe(filter(x => {
      return x[0] && x[1]
    })).subscribe(() => this.getError())
  }

  private getError() {
    const errors = this.formControlName.map(x => {
      const e = this.formGroupDirective.form.get(x.path).errors
      if (e) {
        return {
          path: x.path,
          error: e
        }
      }
    }).filter(x => !!x)

    this.setMessage(errors)
    if (Object.keys(errors).length) {
      this.renderder.addClass(this.elmRef.nativeElement, 'has-error')
    } else {
      this.renderder.removeClass(this.elmRef.nativeElement, 'has-error')
    }
  }

  private setMessage(errors?: {path: string[], error: any}[]) {
    const label = this.elmRef.nativeElement.querySelector('label')
    if (!label) {
      return
    }

    if (!errors) {
      this.renderder.setAttribute(label, 'title', '')
    }

    const message = Array.prototype.concat.call([], errors).map(({path, error}) => {
      return `Control ${path.join('.')} is invalid: ${Object.keys(error).join(',')}`
    }).join('\n')
    this.renderder.setAttribute(label, 'title', message)
  }
}
