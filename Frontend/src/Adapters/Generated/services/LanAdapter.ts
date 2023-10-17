/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type {CreateLanRequest} from '../models/CreateLanRequest';
import type {CreateLanResponse} from '../models/CreateLanResponse';
import type {GetLanResponse} from '../models/GetLanResponse';
import type {UpdateLanRequest} from '../models/UpdateLanRequest';

import type {CancelablePromise} from '../core/CancelablePromise';
import {OpenAPI} from '../core/OpenAPI';
import {request as __request} from '../core/request';

export class LanAdapter {

  /**
   * @param requestBody
   * @returns CreateLanResponse Success
   * @throws ApiError
   */
  public static createLan(
    requestBody?: CreateLanRequest,
  ): CancelablePromise<CreateLanResponse> {
    return __request(OpenAPI, {
      method: 'POST',
      url: '/lan',
      body: requestBody,
      mediaType: 'application/json',
    });
  }

  /**
   * @returns GetLanResponse Success
   * @throws ApiError
   */
  public static getAllLan(): CancelablePromise<Array<GetLanResponse>> {
    return __request(OpenAPI, {
      method: 'GET',
      url: '/lan',
    });
  }

  /**
   * @param id
   * @returns GetLanResponse Success
   * @throws ApiError
   */
  public static getLan(
    id: string,
  ): CancelablePromise<GetLanResponse> {
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
  public static updateLan(
    id: string,
    requestBody?: UpdateLanRequest,
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
