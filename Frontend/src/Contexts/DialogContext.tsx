import React, { useContext } from "react"

export interface DialogResponse<T> {
  positive: boolean
  metadata: T
}

export interface DialogModel<T> {
  title: string
  description: string
  metadata: T
  positiveText: string
  negativeText: string
  resolve: (response: DialogResponse<T>) => void
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  reject: (reason: any) => void
}

interface DialogContextObject {
  showDialog: <T>(
    title: string,
    description: string,
    metadata: T,
    positiveText: string,
    negativeText: string
  ) => Promise<DialogResponse<T>>
}

const defaultValue: DialogContextObject = {
  showDialog: async function <T>(): Promise<DialogResponse<T>> {
    return {
      metadata: null as T,
      positive: false,
    }
  },
}

export const DialogContext =
  React.createContext<DialogContextObject>(defaultValue)

export const useDialogs = () => useContext(DialogContext)
