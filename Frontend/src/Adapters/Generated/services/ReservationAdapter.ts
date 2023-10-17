/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { MoveReservationRequest } from '../models/MoveReservationRequest';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class ReservationAdapter {

    /**
     * @param lanId 
     * @param seatId 
     * @returns any Success
     * @throws ApiError
     */
    public static createReservation(
lanId: string,
seatId: string,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/lan/{lanId}/seat/{seatId}/reservation',
            path: {
                'lanId': lanId,
                'seatId': seatId,
            },
        });
    }

    /**
     * @param lanId 
     * @param seatId 
     * @returns any Success
     * @throws ApiError
     */
    public static deleteReservation(
lanId: string,
seatId: string,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/lan/{lanId}/seat/{seatId}/reservation',
            path: {
                'lanId': lanId,
                'seatId': seatId,
            },
        });
    }

    /**
     * @param lanId 
     * @param seatId 
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static moveReservation(
lanId: string,
seatId: string,
requestBody?: MoveReservationRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/lan/{lanId}/seat/{seatId}/reservation',
            path: {
                'lanId': lanId,
                'seatId': seatId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }

}
