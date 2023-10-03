/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ReservationCreateRequest } from '../models/ReservationCreateRequest';
import type { ReservationMoveRequest } from '../models/ReservationMoveRequest';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class ReservationAdapter {

    /**
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static seatpickerInfrastructureEntrypointsHttpReservationCreateEndpointInfrastructure(
requestBody?: ReservationCreateRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/reservation',
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
    public static seatpickerInfrastructureEntrypointsHttpReservationMoveEndpointInfrastructure(
id: string,
requestBody?: ReservationMoveRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/reservation/{id}',
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
    public static seatpickerInfrastructureEntrypointsHttpReservationRemoveEndpointInfrastructure(
id: string,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/reservation/{id}',
            path: {
                'id': id,
            },
        });
    }

}
