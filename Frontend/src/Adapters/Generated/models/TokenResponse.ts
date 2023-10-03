/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { Role } from './Role';

export type TokenResponse = {
    token?: string;
    userId?: string;
    nick?: string;
    avatar?: string | null;
    roles?: Array<Role>;
};
