import { Close } from "@mui/icons-material"
import {
  Dialog,
  DialogContent,
  DialogTitle,
  IconButton,
  useMediaQuery,
  useTheme,
} from "@mui/material"
import React, { PropsWithChildren } from "react"

export default function Modal(
  props: {
    title: string
    open: boolean
    onClose: () => void
  } & PropsWithChildren
) {
  const theme = useTheme()
  const fullScreen = useMediaQuery(theme.breakpoints.down("md"))

  return (
    <React.Fragment>
      <Dialog
        fullScreen={fullScreen}
        onClose={props.onClose}
        open={props.open}
        sx={{ for: "red" }}
        PaperProps={{
          sx: { backgroundColor: theme.palette.background.default },
        }}
      >
        <DialogTitle>{props.title}</DialogTitle>
        <IconButton
          aria-label="close"
          onClick={props.onClose}
          sx={{
            position: "absolute",
            right: 8,
            top: 8,
          }}
        >
          <Close />
        </IconButton>
        <DialogContent dividers>{props.children}</DialogContent>
      </Dialog>
    </React.Fragment>
  )
}
