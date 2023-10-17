/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { GetDiscordAuthenticationRoleMappingResponse } from '../models/GetDiscordAuthenticationRoleMappingResponse';
import type { LoginDiscordAuthenticationRequest } from '../models/LoginDiscordAuthenticationRequest';
import type { LoginDiscordAuthenticationResponse } from '../models/LoginDiscordAuthenticationResponse';
import type { RenewDiscordAuthenticationRequest } from '../models/RenewDiscordAuthenticationRequest';
import type { RenewDiscordAuthenticationResponse } from '../models/RenewDiscordAuthenticationResponse';
import type { SetDiscordAuthenticationRequest } from '../models/SetDiscordAuthenticationRequest';
import type { TestDiscordAuthenticationResponse } from '../models/TestDiscordAuthenticationResponse';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class DiscordAuthenticationAdapter {

    /**
     * @param guildId
     * @returns GetDiscordAuthenticationRoleMappingResponse Success
     * @throws ApiError
     */
    public static setDiscordAuthentication(
guildId: string,
): CancelablePromise<Array<GetDiscordAuthenticationRoleMappingResponse>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/authentication/discord/roles/{guildId}',
            path: {
                'guildId': guildId,
            },
        });
    }

    /**
     * @param guildId
     * @param requestBody
     * @returns any Success
     * @throws ApiError
     */
    public static setDiscordAuthentication1(
guildId: string,
requestBody?: SetDiscordAuthenticationRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/authentication/discord/roles/{guildId}',
            path: {
                'guildId': guildId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param requestBody
     * @returns LoginDiscordAuthenticationResponse Success
     * @throws ApiError
     */
    public static login(
requestBody?: LoginDiscordAuthenticationRequest,
): CancelablePromise<LoginDiscordAuthenticationResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/authentication/discord/login',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param requestBody
     * @returns RenewDiscordAuthenticationResponse Success
     * @throws ApiError
     */
    public static loginDiscordAuthentication1(
requestBody?: RenewDiscordAuthenticationRequest,
): CancelablePromise<RenewDiscordAuthenticationResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/authentication/discord/renew',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @returns TestDiscordAuthenticationResponse Success
     * @throws ApiError
     */
    public static testDiscordAuthentication(): CancelablePromise<TestDiscordAuthenticationResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/authentication/discord/test',
        });
    }

}
