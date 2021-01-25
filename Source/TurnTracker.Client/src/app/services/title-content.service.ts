import { Injectable } from '@angular/core';
import { Title } from '@angular/platform-browser';

@Injectable({
  providedIn: 'root'
})
export class TitleContentService {

  constructor(private titleService: Title) { }

  /**
   * Attempt to append content to the title if it's not already there.
   * @param content The custom content string to include in the title.
   */
  public setTitleContent(content: string) {
    if(!content) {
      return;
    }

    var title = this.titleService.getTitle();
    if(!title.endsWith(content)) {
      this.titleService.setTitle(`${title} - ${content}`)
    }
  }
}
