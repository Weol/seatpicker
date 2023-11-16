import {
  Dialog as MuiDialog,
  Button,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
} from "@mui/material"
import React, { useContext } from "react"

export interface DialogResponse<T> {
  positive: boolean
  metadata: T
}

export interface DialogModel {
  title: string
  description: string
  metadata: never
  positiveText: string
  negativeText: string
  resolve: (response: DialogResponse<never>) => void
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

export function setupDialogs(setDialog: (dialog: DialogModel | null) => void) {
  function Dialog(props: { dialog: DialogModel }) {
    return (
      <MuiDialog
        open={true}
        onClose={() => setDialog(null)}
        aria-labelledby="alert-dialog-title"
        aria-describedby="alert-dialog-description"
      >
        <DialogTitle id="alert-dialog-title">{props.dialog.title}</DialogTitle>
        <DialogContent>
          <DialogContentText id="alert-dialog-description">
            {props.dialog.description}
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button
            onClick={() => {
              props.dialog.resolve({
                positive: false,
                metadata: props.dialog.metadata,
              })

              setDialog(null)
            }}
          >
            {props.dialog.negativeText}
          </Button>
          <Button
            onClick={() => {
              props.dialog.resolve({
                positive: true,
                metadata: props.dialog.metadata,
              })

              setDialog(null)
            }}
          >
            {props.dialog.positiveText}
          </Button>
        </DialogActions>
      </MuiDialog>
    )
  }

  function showDialog<T>(
    title: string,
    description: string,
    metadata: T,
    positiveText: string,
    negativeText: string
  ): Promise<DialogResponse<T>> {
    return new Promise<DialogResponse<T>>((resolve, reject) => {
      setDialog({
        title: title,
        description: description,
        metadata: metadata as never,
        positiveText: positiveText,
        negativeText: negativeText,
        resolve: resolve,
        reject: reject,
      })
    })
  }

  return { Dialog, showDialog }
}
