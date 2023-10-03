/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { FrontendGetAllSeatsResponse } from '../models/FrontendGetAllSeatsResponse';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class FrontendAdapter {

    /**
     * @returns FrontendGetAllSeatsResponse Success
     * @throws ApiError
     */
    public static seatpickerInfrastructureEntrypointsHttpFrontendGetAllSeatsGetInfrastructure(): CancelablePromise<FrontendGetAllSeatsResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/frontend/seats',
        });
    }

}
