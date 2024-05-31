export class AdapterError extends Error {
  public code: number

  constructor(code: number) {
    super()
    this.code = code
  }
}

export class NotFoundError extends AdapterError {
  constructor() {
    super(404)
  }
}
