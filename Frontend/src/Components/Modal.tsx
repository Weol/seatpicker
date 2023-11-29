import { Modal as MuiModal, Box } from "@mui/material"
import React, { PropsWithChildren } from "react"

export default function Modal(
  props: {
    maxWidth?: string
    open: boolean
    onClose: () => void
  } & PropsWithChildren
) {
  return (
    <MuiModal {...props}>
      <Box
        sx={{
          position: "absolute",
          top: "50%",
          left: "50%",
          transform: "translate(-50%, -50%)",
          maxWidth: ["90%", "90%", "50%"],
          bgcolor: "background.paper",
          p: 4,
        }}
      >
        {props.children}
      </Box>
    </MuiModal>
  )
}
