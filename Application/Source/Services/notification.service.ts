﻿import { Injectable } from '@angular/core';
import { from } from 'rxjs';
import { NotificationManager } from '../Managers/notification.manager';


@Injectable()
export class NotificationService {
    constructor() { }

    public get isSubscribed(): boolean {
        return Notification.permission == "granted";
    }

    public get isNotAsked(): boolean {
        return Notification.permission == "default";
    }

    public subscribe(): void {
        from(Notification.requestPermission()).subscribe((value: NotificationPermission) => {
            if (value == "granted") {
                this.generalNotification("Notifications Successfully Subscribed", () => {
                    window.location.reload();
                });
            }
        });
    }

    public offline(): void {
        this.generalNotification("Application is Offline");
    }

    public online(): void {
        this.generalNotification("Application is Online");
    }

    public generalNotification(body: string, eventCallBack?: () => void): void {
        let options: NotificationOptions = {
            body: body,
            icon: NotificationManager.iconUrl
        }
        let notification = new Notification(NotificationManager.title, options);
        notification.addEventListener("click", (event: Event) => {
            if (eventCallBack) {
                eventCallBack();
            }
        });
    }
}