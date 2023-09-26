/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateLanRequestModel } from '../models/CreateLanRequestModel';
import type { UpdateLanRequestModel } from '../models/UpdateLanRequestModel';

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
requestBody?: CreateLanRequestModel,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/Lan',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getLan(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/Lan',
        });
    }

    /**
     * @param id 
     * @returns any Success
     * @throws ApiError
     */
    public static getLan1(
id: string,
): CancelablePromise<any> {
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
requestBody?: UpdateLanRequestModel,
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
