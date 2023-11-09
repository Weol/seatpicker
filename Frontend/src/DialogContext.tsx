import React, {useContext} from "react";

export interface Dialog<T> {
  title: string;
  description: string;
  positiveText: string;
  negativeText?: string;
  positiveCallback?: (data: T) => void;
  negativeCallback?: (data: T) => void;
  metadata?: T;
}

interface DialogContextObject {
  showDialog: <T>(dialog: Dialog<T>) => void
}

function showDialog<T>(dialog : Dialog<T>) {}

let defaultValue: DialogContextObject = {
  showDialog: showDialog
}

export const DialogContext = React.createContext<DialogContextObject>(defaultValue);

export const useDialogs = () => useContext(DialogContext)