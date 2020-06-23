import {NgModule} from '@angular/core'
import {CommonModule} from '@angular/common'
import {FormGroupClassDirective} from './form-group-class.directive'
import {BreadcrumbDirective} from './breadcrumb.directive'
import {BreadcrumbComponent} from './breadcrumb/breadcrumb.component'
import {RouterModule} from '@angular/router'
import {DropdownDirective, DropdownMenuDirective, DropdownToggleDirective} from './dropdown.directive'

@NgModule({
  declarations: [
    FormGroupClassDirective,
    BreadcrumbDirective,
    BreadcrumbComponent,
    DropdownDirective,
    DropdownToggleDirective,
    DropdownMenuDirective
  ],
  exports: [
    FormGroupClassDirective,
    BreadcrumbDirective,
    BreadcrumbComponent,
    DropdownDirective,
    DropdownToggleDirective,
    DropdownMenuDirective
  ],
  imports: [
    CommonModule,
    RouterModule,
  ],
})
export class SharedModule {
}
