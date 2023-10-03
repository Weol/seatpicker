/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { LanCreateRequest } from '../models/LanCreateRequest';
import type { LanGetResponse } from '../models/LanGetResponse';
import type { LanUpdateRequest } from '../models/LanUpdateRequest';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class LanAdapter {

    /**
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static seatpickerInfrastructureEntrypointsHttpLanCreateEndpointInfrastructure(
requestBody?: LanCreateRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/lan',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param id 
     * @returns LanGetResponse Success
     * @throws ApiError
     */
    public static seatpickerInfrastructureEntrypointsHttpLanGetEndpointInfrastructure(
id: string,
): CancelablePromise<LanGetResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/lan/{id}',
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
    public static seatpickerInfrastructureEntrypointsHttpLanUpdateEndpointInfrastructure(
id: string,
requestBody?: LanUpdateRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/lan/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }

}
