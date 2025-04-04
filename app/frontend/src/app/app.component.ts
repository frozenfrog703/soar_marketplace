import { Component, OnInit } from '@angular/core';
import { Alert, AlertService } from './alert.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  imports: [CommonModule]
})
export class AppComponent implements OnInit {
  alerts: Alert[] = [];

  constructor(private alertService: AlertService) { }

  ngOnInit(): void {
    this.loadAlerts();
    // Optionally, set up an interval to refresh alerts.
    setInterval(() => this.loadAlerts(), 30000); // refresh every 30 seconds
  }

  loadAlerts() {
    this.alertService.getAlerts().subscribe(data => {
      this.alerts = data;
    });
  }
}
