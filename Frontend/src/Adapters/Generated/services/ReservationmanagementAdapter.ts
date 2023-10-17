/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateReservationManagementRequest } from '../models/CreateReservationManagementRequest';
import type { MoveReservationManagementRequest } from '../models/MoveReservationManagementRequest';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class ReservationmanagementAdapter {

    /**
     * @param lanId 
     * @param seatId 
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static createReservationmanagement(
lanId: string,
seatId: string,
requestBody?: CreateReservationManagementRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/lan/{lanId}/seat/{seatId}/reservationmanagement',
            path: {
                'lanId': lanId,
                'seatId': seatId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param lanId 
     * @param seatId 
     * @returns any Success
     * @throws ApiError
     */
    public static deleteReservationmanagement(
lanId: string,
seatId: string,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/lan/{lanId}/seat/{seatId}/reservationmanagement',
            path: {
                'lanId': lanId,
                'seatId': seatId,
            },
        });
    }

    /**
     * @param lanId 
     * @param seatId 
     * @param id 
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static moveReservationmanagement(
lanId: string,
seatId: string,
id: string,
requestBody?: MoveReservationManagementRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/lan/{lanId}/seat/{seatId}/reservationmanagement/{id}',
            path: {
                'lanId': lanId,
                'seatId': seatId,
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }

}
