/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { SeatCreateRequest } from '../models/SeatCreateRequest';
import type { SeatUpdateRequest } from '../models/SeatUpdateRequest';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class SeatAdapter {

    /**
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static seatpickerInfrastructureEntrypointsHttpSeatCreateEndpointInfrastructure(
requestBody?: SeatCreateRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/seat',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param id 
     * @returns any Success
     * @throws ApiError
     */
    public static seatpickerInfrastructureEntrypointsHttpSeatRemoveEndpointInfrastructure(
id: string,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/seat/{id}',
            path: {
                'id': id,
            },
        });
    }

    /**
     * @param id 
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static seatpickerInfrastructureEntrypointsHttpSeatUpdateEndpointInfrastructure(
id: string,
requestBody?: SeatUpdateRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/seat/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }

}
