import { Overlay, OverlayConfig } from '@angular/cdk/overlay';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-reload',
  templateUrl: './reload.component.html',
  styleUrls: ['./reload.component.scss']
})
export class ReloadComponent implements OnInit {

  constructor() { }

  public static BuildOverlayConfig(overlay: Overlay): OverlayConfig {
    return new OverlayConfig({
      hasBackdrop: true,
      backdropClass: 'warn-backdrop',
      positionStrategy: overlay.position().global().centerHorizontally().centerVertically(),
      scrollStrategy: overlay.scrollStrategies.noop()
    })
  }

  ngOnInit(): void {
  }

  public reloadPage() {
    document.location.reload();
  }

}
