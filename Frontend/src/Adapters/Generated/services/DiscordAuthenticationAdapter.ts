/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DiscordRoleMappingRequestModel } from '../models/DiscordRoleMappingRequestModel';
import type { LoginRequestModel } from '../models/LoginRequestModel';
import type { RenewRequestModel } from '../models/RenewRequestModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DiscordAuthenticationAdapter {

    /**
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static postDiscordRenew(
requestBody?: RenewRequestModel,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/discord/renew',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static postDiscordLogin(
requestBody?: LoginRequestModel,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/discord/login',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getDiscordTest(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/discord/test',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getDiscordRoles(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/discord/roles',
        });
    }

    /**
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static putDiscordRoles(
requestBody?: DiscordRoleMappingRequestModel,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/discord/roles',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

}
