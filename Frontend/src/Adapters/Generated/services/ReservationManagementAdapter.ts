/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateReservationForRequestModel } from '../models/CreateReservationForRequestModel';
import type { MoveReservationForRequestModel } from '../models/MoveReservationForRequestModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class ReservationManagementAdapter {

    /**
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static postReservationManagement(
requestBody?: CreateReservationForRequestModel,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/ReservationManagement',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param id 
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static putReservationManagement(
id: string,
requestBody?: MoveReservationForRequestModel,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/ReservationManagement/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param id 
     * @returns any Success
     * @throws ApiError
     */
    public static deleteReservationManagement(
id: string,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/ReservationManagement/{id}',
            path: {
                'id': id,
            },
        });
    }

}
