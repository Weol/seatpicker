// noinspection TypeScriptCheckImport

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
import {useAlertContext} from "../AlertContext";

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
  onSave: (lan: Lan | null, title: string, background: Blob) => void
  onCancel: () => void
  onDelete: (lan : Lan) => void
}

const LanEditor = (params: LanEditorParams) => {
  let {setAlert} = useAlertContext()

  let [title, setTitle] = useState<string | null>(params.lan && params.lan.title || null)
  let [background, setBackground] = useState<Blob | null>(null)
  let [backgroundName, setBackgroundName] = useState<string | null>(null)
  let [viewBackground, setViewBackground] = useState<boolean>(false)
  
  const onCancelPressed = () => {
    setTitle(null)
    setBackground(null)
    setBackgroundName(null)
  }
  
  const onSavePressed = () => {
  
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
          setBackground(new Blob([bytes]))
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
    <Stack spacing={2} justifyContent="center" alignItems="center">
      <TextField sx={{width: "100%"}} required label="Lan name" variant="outlined" onInput={onTitleChanged}/>
      <Button sx={{width: "100%"}} component="label" variant="outlined" startIcon={<UploadIcon/>}>
        <Typography noWrap={true}>{backgroundName || "Upload background"}</Typography>
        <VisuallyHiddenInput type="file" onInput={onBackgroundChanged} accept="image/svg"/>
      </Button>
      {!viewBackground && background && <Button onClick={() => setViewBackground(true)} startIcon={<FullscreenIcon/>}>View backgroud</Button>}
      {viewBackground && background && <Button onClick={() => setViewBackground(false)} startIcon={<FullscreenExitIcon/>}>Hide backgroud</Button>}
      {viewBackground && background && <img alt="preview" style={{width: "100%"}} src={URL.createObjectURL(background)}/>}
      <Stack sx={{width: "100%"}} spacing={1} direction="row" justifyContent="space-between" alignItems="center">
        <Button onClick={onSavePressed} color="secondary" variant="contained">
          {params.lan && "Update" || "Create"}
        </Button>
        <IconButton aria-label="cancel" onClick={onCancelPressed}>
          {params.lan &&  <DeleteIcon/> || <CancelIcon/>}
        </IconButton>
      </Stack>
    </Stack>
  )
}

export default function Admin() {
  let {setAlert} = useAlertContext()
  const [createExpanded, setCreateExpanded] = useState<boolean>(false);

  const onSave = (lan: Lan | null, title: string, background: Blob) => {
    if (lan != null) {
      setAlert({
        title: "Lan opprettet",
        type: "success"
      })
    } else {
      setAlert({
        title: "Lan oppdatert",
        type: "success"
      })
    }
  }
  
  const onCancel = () => {
    
  }
  
  const onDelete = (lan : Lan) => {}
  
  return (
    <Stack spacing={1} justifyContent="center" alignItems="center" sx={{marginTop: "1em"}}>
      <Accordion sx={{width: "100%"}}>
        <AccordionSummary aria-controls="panel1d-content" id="panel1d-header" sx={{width: "100%"}}>
          <Typography variant={"h5"} align={"center"} sx={{width: "100%"}}>Create new lan</Typography>
        </AccordionSummary>
        <Divider/>
        <AccordionDetails sx={{paddingTop: "1em"}}>
          <LanEditor onSave={onSave} onCancel={onCancel} onDelete={onDelete} lan={null}/>
        </AccordionDetails>
      </Accordion>
      <Divider/>
    </Stack>
  )
}
