/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DiscordRoleMappingRequest } from '../models/DiscordRoleMappingRequest';
import type { DiscordRoleMappingResponse } from '../models/DiscordRoleMappingResponse';
import type { LoginRequest } from '../models/LoginRequest';
import type { RenewRequest } from '../models/RenewRequest';
import type { TestResponse } from '../models/TestResponse';
import type { TokenResponse } from '../models/TokenResponse';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DiscordAdapter {

    /**
     * @param requestBody 
     * @returns TokenResponse Success
     * @throws ApiError
     */
    public static seatpickerInfrastructureAuthenticationDiscordDiscordAuthenticationControllerRenewInfrastructure(
requestBody?: RenewRequest,
): CancelablePromise<TokenResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/discord/renew',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @param requestBody 
     * @returns TokenResponse Success
     * @throws ApiError
     */
    public static seatpickerInfrastructureAuthenticationDiscordDiscordAuthenticationControllerLoginInfrastructure(
requestBody?: LoginRequest,
): CancelablePromise<TokenResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/discord/login',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns TestResponse Success
     * @throws ApiError
     */
    public static seatpickerInfrastructureAuthenticationDiscordDiscordAuthenticationControllerTestInfrastructure(): CancelablePromise<TestResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/discord/test',
        });
    }

    /**
     * @returns DiscordRoleMappingResponse Success
     * @throws ApiError
     */
    public static seatpickerInfrastructureAuthenticationDiscordDiscordAuthenticationControllerGetRolesInfrastructure(): CancelablePromise<Array<DiscordRoleMappingResponse>> {
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
    public static seatpickerInfrastructureAuthenticationDiscordDiscordAuthenticationControllerPutRolesInfrastructure(
requestBody?: DiscordRoleMappingRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/discord/roles',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

}
