export class AdapterError extends Error {
  public code: number
  public message: string

  constructor(code: number, message: string) {
    super()
    this.code = code
    this.message = message
  }
}

export class BadRequestError extends AdapterError {
  constructor(code: 400 | 422 | 405 | 415, message: string) {
    super(code, message)
  }
}

export class NotFoundError extends AdapterError {
  constructor(message: string) {
    super(404, message)
  }
}

export class AuthenticationError extends AdapterError {
  constructor(code: 401 | 403, message: string) {
    super(code, message)
  }
}

export class ConflictError extends AdapterError {
  constructor(message: string) {
    super(409, message)
  }
}

export class InternalServerError extends AdapterError {
  constructor(code: number, message: string) {
    super(500, message)
  }
}
