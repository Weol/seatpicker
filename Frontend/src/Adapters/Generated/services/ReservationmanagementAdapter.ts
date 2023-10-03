/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ReservationManagementCreateRequest } from '../models/ReservationManagementCreateRequest';
import type { ReservationManagementMoveRequest } from '../models/ReservationManagementMoveRequest';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class ReservationmanagementAdapter {

    /**
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static seatpickerInfrastructureEntrypointsHttpReservationManagementCreateEndpointInfrastructure(
requestBody?: ReservationManagementCreateRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/reservationmanagement',
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
    public static seatpickerInfrastructureEntrypointsHttpReservationManagementMoveEndpointInfrastructure(
id: string,
requestBody?: ReservationManagementMoveRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/reservationmanagement/{id}',
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
    public static seatpickerInfrastructureEntrypointsHttpReservationManagementRemoveEndpointInfrastructure(
id: string,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/reservationmanagement/{id}',
            path: {
                'id': id,
            },
        });
    }

}
