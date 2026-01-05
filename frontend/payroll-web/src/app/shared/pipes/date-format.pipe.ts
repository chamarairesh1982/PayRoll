import { DatePipe } from '@angular/common';
import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'dateFormat' })
export class DateFormatPipe implements PipeTransform {
  private datePipe = new DatePipe('en-US');

  transform(value: string | Date | null | undefined, format = 'mediumDate'): string {
    if (!value) {
      return '';
    }
    return this.datePipe.transform(value, format) ?? '';
  }
}
