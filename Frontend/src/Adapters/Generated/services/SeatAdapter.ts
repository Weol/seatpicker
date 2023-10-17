/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateSeatRequest } from '../models/CreateSeatRequest';
import type { CreateSeatResponse } from '../models/CreateSeatResponse';
import type { GetSeatResponse } from '../models/GetSeatResponse';
import type { UpdateSeatRequest } from '../models/UpdateSeatRequest';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class SeatAdapter {

    /**
     * @param lanId 
     * @param requestBody 
     * @returns CreateSeatResponse Success
     * @throws ApiError
     */
    public static createSeat(
lanId: string,
requestBody?: CreateSeatRequest,
): CancelablePromise<CreateSeatResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/lan/{lanId}/seat',
            path: {
                'lanId': lanId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param lanId 
     * @returns GetSeatResponse Success
     * @throws ApiError
     */
    public static getAllSeat(
lanId: string,
): CancelablePromise<Array<GetSeatResponse>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/lan/{lanId}/seat',
            path: {
                'lanId': lanId,
            },
        });
    }

    /**
     * @param lanId 
     * @param seatId 
     * @returns any Success
     * @throws ApiError
     */
    public static deleteSeat(
lanId: string,
seatId: string,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/lan/{lanId}/seat/{seatId}',
            path: {
                'lanId': lanId,
                'seatId': seatId,
            },
        });
    }

    /**
     * @param lanId 
     * @param seatId 
     * @returns GetSeatResponse Success
     * @throws ApiError
     */
    public static getSeat(
lanId: string,
seatId: string,
): CancelablePromise<GetSeatResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/lan/{lanId}/seat/{seatId}',
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
    public static updateSeat(
lanId: string,
seatId: string,
requestBody?: UpdateSeatRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/lan/{lanId}/seat/{seatId}',
            path: {
                'lanId': lanId,
                'seatId': seatId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }

}
