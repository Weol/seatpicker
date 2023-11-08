import * as React from 'react';
import {
  Accordion, AccordionDetails,
  AccordionSummary,
  Stack, styled, TextField
} from '@mui/material';
import UploadIcon from '@mui/icons-material/Upload';
import CancelIcon from '@mui/icons-material/Cancel';
import DeleteIcon from '@mui/icons-material/Delete';
import FullscreenIcon from '@mui/icons-material/Fullscreen';
import FullscreenExitIcon from '@mui/icons-material/FullscreenExit';
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";
import Divider from "@mui/material/Divider";
import IconButton from "@mui/material/IconButton";
import Lan from "../Models/Lan";
import {useState} from "react";
import {useAlerts} from "../AlertContext";
import {Add} from '@mui/icons-material';
import useLans from "../LanHook";

const VisuallyHiddenInput = styled('input')({
  clip: 'rect(0 0 0 0)',
  clipPath: 'inset(50%)',
  height: 1,
  overflow: 'hidden',
  position: 'absolute',
  bottom: 0,
  left: 0,
  whiteSpace: 'nowrap',
  width: 1,
});

interface LanEditorParams {
  lan: Lan | null;
  onSave: (lan: Lan | null, title: string, background: string) => void
  onCancel: () => void
  onDelete: (lan: Lan) => void
}

const LanEditor = (params: LanEditorParams) => {
  let { setAlert } = useAlerts()

  const getDefaultBackground = () => {
    if (params.lan != null) {
      return new Blob([ atob(params.lan.background) ], {type: "image/svg+xml"})
    }
    return null
  }

  let [ title, setTitle ] = useState<string | null>(params.lan && params.lan.title || null)
  let [ background, setBackground ] = useState<Blob | null>(getDefaultBackground())
  let [ backgroundName, setBackgroundName ] = useState<string | null>(null)
  let [ viewBackground, setViewBackground ] = useState<boolean>(false)

  const onCancelPressed = () => {
    if (params.lan != null) {
      params.onDelete(params.lan)
    } else {
      params.onCancel()
    }

    setTitle(null)
    setBackground(null)
    setBackgroundName(null)
  }

  const blobToBase64 = (blob: Blob): Promise<string> => {
    const reader = new FileReader();
    reader.readAsDataURL(blob);
    return new Promise<string>(resolve => {
      reader.onloadend = () => {
        let result = reader.result as string
        if (result != null) {
          resolve(result.split(',')[1]);
        }
      };
    });
  };

  const onSavePressed = () => {
    if (background != null) {
      blobToBase64(background)
      .then((base64Background) => {
        if (title != null) {
          params.onSave(params.lan, title, base64Background)
        }
      })
    }
  }

  const onTitleChanged = (event: React.ChangeEvent<HTMLInputElement>) => {
    let value = event.target.value
    if (value.length > 0) {
      setTitle(event.target.value)
    } else {
      setTitle(null)
    }
  }

  const onBackgroundChanged = (event: React.ChangeEvent<HTMLInputElement>) => {
    let files = event.target.files;
    if (files != null) {
      let file = files[0]
      let reader = new FileReader()

      reader.onload = function (e) {
        if (e.target != null) {
          let bytes = e.target.result as ArrayBuffer
          setBackground(new Blob([ bytes ], {type: "image/svg+xml"}))
          setBackgroundName(file.name)
        } else {
          setAlert({
            type: "error",
            title: "Kunne ikke lese filen"
          })
        }
      };

      reader.onerror = function (e) {
        setAlert({
          type: "error",
          title: "Kunne ikke lese filen"
        })
      }

      reader.readAsArrayBuffer(file)
    }
  }
  // {<img alt="preview" style={{width: "100%"}} src={URL.createObjectURL(background)}/>}
  return (
    <Stack spacing={2} justifyContent="center" alignItems="center" sx={{ width: "100%" }}>
      <TextField defaultValue={title} sx={{ width: "100%" }} required label="Lan name" variant="outlined" onInput={onTitleChanged}/>
      <Button sx={{ width: "100%" }} component="label" variant="outlined" startIcon={<UploadIcon/>}>
        <Typography noWrap={true}>{background && "Change background" || backgroundName || "Upload background"}</Typography>
        <VisuallyHiddenInput type="file" onInput={onBackgroundChanged} accept="image/svg"/>
      </Button>
      {!viewBackground && background &&
          <Button onClick={() => setViewBackground(true)} startIcon={<FullscreenIcon/>}>View backgroud</Button>}
      {viewBackground && background &&
          <Button onClick={() => setViewBackground(false)} startIcon={<FullscreenExitIcon/>}>Hide backgroud</Button>}
      {viewBackground && background &&
          <img alt="preview" style={{ width: "100%"}} src={URL.createObjectURL(background)}/>}
      <Stack sx={{ width: "100%" }} spacing={1} direction="row" justifyContent="space-between" alignItems="center">
        <Button onClick={onSavePressed} color="secondary" variant="contained">
          {params.lan && "Update" || "Create"}
        </Button>
        <IconButton aria-label="cancel" onClick={onCancelPressed}>
          {params.lan && <DeleteIcon/> || <CancelIcon/>}
        </IconButton>
      </Stack>
    </Stack>
  )
}

export default function Admin() {
  let { setAlert } = useAlerts()
  let { lans, reloadLans, createNewLan, deleteLan } = useLans()
  let [ isCreateVisible, setCreateVisible ] = useState<boolean>(false);
  let [ expanded, setExpanded ] = useState<string | false>(false);

  const handleLanPressed = (lan: string) => (event: React.SyntheticEvent, newExpanded: boolean) => {
    setExpanded(newExpanded ? lan : false);
  };

  const onSave = (lan: Lan | null, title: string, background: string) => {
    if (lan != null) {
      setAlert({
        title: "Lan opprettet",
        type: "success"
      })
    } else {
      createNewLan(title, background)
        .then((response) => {
          reloadLans()
          setAlert({
            title: title,
            type: "success"
          })
        })
    }
  }

  const onCancel = () => {
    setCreateVisible(false)
  }

  const onDelete = (lan: Lan) => {
    deleteLan(lan)
      .then(response => {
        setAlert({
          title: "Lan slettet",
          type: "success"
        })
        reloadLans()
      })
  }

  return (
    <Stack divider={<Divider orientation="horizontal" flexItem />} spacing={2} justifyContent="center" alignItems="center" sx={{ marginTop: "1em" }}>
      <Stack direction="row" width="100%" justifyContent="space-between">
        <Typography variant="h4">Alle lan</Typography>
        <Button onClick={() => setCreateVisible(true)} startIcon={<Add/>} variant="outlined" color="secondary">Nytt
          lan</Button>
      </Stack>
      {isCreateVisible && <LanEditor onSave={onSave} onCancel={onCancel} onDelete={onDelete} lan={null}/>}
      <Stack justifyContent="center" alignItems="center" sx={{ marginTop: "1em" }} width="100%">
        {lans.map(lan => (
          <Accordion expanded={expanded == lan.id} onChange={handleLanPressed(lan.id)} sx={{ width: "100%" }}>
            <AccordionSummary sx={{ width: "100%" }}>
              <Typography>{lan.title}</Typography>
            </AccordionSummary>
            <Divider orientation="horizontal" flexItem/>
            <AccordionDetails sx={{ paddingTop: "1em" }}>
              <LanEditor lan={lan} onSave={onSave} onCancel={onCancel} onDelete={onDelete}/>
            </AccordionDetails>
          </Accordion>
        ))}
      </Stack>
    </Stack>
  )
}
