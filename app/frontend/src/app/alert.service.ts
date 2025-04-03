import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface IoC {
  identifier: string;
  isMalicious: boolean;
  country: string;
  lastModificationDate: string;
  lastAnalysisStats: {
    harmless: number;
    malicious: number;
    suspicious: number;
    timeout: number;
    undetected: number;
  };
  tags: string[];
  reportLink: string;
}

export interface Alert {
  alertId: string;
  severity: number;
  ioCs: IoC[];
  receivedAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class AlertService {

  private apiUrl = 'https://localhost:7044/api/alerts'; // adjust the port as needed

  constructor(private http: HttpClient) { }

  getAlerts(): Observable<Alert[]> {
    return this.http.get<Alert[]>(this.apiUrl);
  }
}
