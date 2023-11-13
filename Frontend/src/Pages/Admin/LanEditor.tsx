import {
  Button,
  IconButton,
  Stack,
  TextField,
  Typography,
  styled,
} from "@mui/material"
import { ReactNode, useState } from "react"
import { useAlerts } from "../../Contexts/AlertContext"
import { Fullscreen, FullscreenExit, Upload } from "@mui/icons-material"
import { Lan } from "../../Adapters/LanAdapter"

const VisuallyHiddenInput = styled("input")({
  clip: "rect(0 0 0 0)",
  clipPath: "inset(50%)",
  height: 1,
  overflow: "hidden",
  position: "absolute",
  bottom: 0,
  left: 0,
  whiteSpace: "nowrap",
  width: 1,
})

interface LanEditorParams {
  lan?: Lan
  saveButtonText: string
  cancelButtonIcon: ReactNode
  backgroundButtonText: string
  onSave: (title: string, background: string) => void
  onCancel: () => void
}

const blobToBase64 = (blob: Blob): Promise<string> => {
  const reader = new FileReader()
  reader.readAsDataURL(blob)
  return new Promise<string>((resolve) => {
    reader.onloadend = () => {
      const result = reader.result as string
      if (result != null) {
        resolve(result.split(",")[1])
      }
    }
  })
}

const base64ToBlob = (base64: string): Blob => {
  return new Blob([atob(base64)], { type: "image/svg+xml" })
}

export default function LanEditor(params: LanEditorParams) {
  const { alertError } = useAlerts()
  const [title, setTitle] = useState<string | null>(
    params.lan !== undefined ? params.lan.title : null
  )
  const [background, setBackground] = useState<Blob | null>(
    params.lan !== undefined ? base64ToBlob(params.lan.background) : null
  )
  const [backgroundName, setBackgroundName] = useState<string | null>(null)
  const [viewBackground, setViewBackground] = useState<boolean>(false)
  const [errors, setErrors] = useState<{ title: boolean; background: boolean }>(
    {
      title: false,
      background: false,
    }
  )

  const onCancelPressed = () => {
    params.onCancel()
  }

  const onSavePressed = async () => {
    setErrors({
      title: title == null || title.length == 0,
      background: background == null || background.size == 0,
    })

    if (title != null && background != null) {
      const base64 = await blobToBase64(background)

      params.onSave(title, base64)
    }
  }

  const onTitleChanged = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = event.target.value
    if (value.length > 0) {
      setTitle(value)
    } else {
      setTitle(null)
    }
  }

  const onBackgroundChanged = (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = event.target.files
    if (files != null) {
      const file = files[0]
      const reader = new FileReader()

      reader.onload = function (e) {
        if (e.target != null) {
          const bytes = e.target.result as ArrayBuffer
          setBackground(new Blob([bytes], { type: "image/svg+xml" }))
          setBackgroundName(file.name)
        } else {
          alertError("Kunne ikke lese filen")
        }
      }

      reader.onerror = function () {
        alertError("Kunne ikke lese filen")
      }

      reader.readAsArrayBuffer(file)
    }
  }

  return (
    <Stack
      spacing={2}
      justifyContent="center"
      alignItems="center"
      sx={{ width: "100%" }}
    >
      <TextField
        defaultValue={title}
        sx={{ width: "100%" }}
        required
        label="Lan name"
        variant="outlined"
        onChange={onTitleChanged}
        error={errors.title}
      />
      <Button
        sx={{ width: "100%" }}
        color={(errors.background && "error") || "primary"}
        component="label"
        variant="outlined"
        startIcon={<Upload />}
      >
        <Typography noWrap={true}>
          {backgroundName || params.backgroundButtonText}
        </Typography>
        <VisuallyHiddenInput
          type="file"
          onInput={onBackgroundChanged}
          accept="image/svg"
        />
      </Button>
      {!viewBackground && background && (
        <Button
          onClick={() => setViewBackground(true)}
          startIcon={<Fullscreen />}
        >
          View background
        </Button>
      )}
      {viewBackground && background && (
        <Button
          onClick={() => setViewBackground(false)}
          startIcon={<FullscreenExit />}
        >
          Hide background
        </Button>
      )}
      {viewBackground && background && (
        <img
          alt="preview"
          style={{ width: "100%" }}
          src={URL.createObjectURL(background)}
        />
      )}
      <Stack
        sx={{ width: "100%" }}
        spacing={1}
        direction="row"
        justifyContent="space-between"
        alignItems="center"
      >
        <Button onClick={onSavePressed} color="secondary" variant="contained">
          {params.saveButtonText}
        </Button>
        <IconButton aria-label="cancel" onClick={onCancelPressed}>
          {params.cancelButtonIcon}
        </IconButton>
      </Stack>
    </Stack>
  )
}
