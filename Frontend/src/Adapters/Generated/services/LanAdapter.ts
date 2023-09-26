/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateLanRequest } from '../models/CreateLanRequest';
import type { LanResponse } from '../models/LanResponse';
import type { UpdateLanRequest } from '../models/UpdateLanRequest';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class LanAdapter {

    /**
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static postLan(
requestBody?: CreateLanRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/Lan',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns LanResponse Success
     * @throws ApiError
     */
    public static getLan(): CancelablePromise<Array<LanResponse>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/Lan',
        });
    }

    /**
     * @param id 
     * @returns LanResponse Success
     * @throws ApiError
     */
    public static getLan1(
id: string,
): CancelablePromise<LanResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/Lan/{id}',
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
    public static putLan(
id: string,
requestBody?: UpdateLanRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/Lan/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }

}
