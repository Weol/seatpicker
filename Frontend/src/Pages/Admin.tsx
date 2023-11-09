import * as React from 'react';
import {
  Accordion, AccordionDetails,
  AccordionSummary, CircularProgress,
  Stack, styled, SvgIconTypeMap, TextField
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
import {ReactNode, useEffect, useState} from "react";
import {useAlerts} from "../AlertContext";
import {Add, SvgIconComponent} from '@mui/icons-material';
import useLans from "../LanHook";
import DelayedCircularProgress from '../Components/DelayedCircularProgress';
import {useDialogs} from "../DialogContext";

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
  lan?: Lan;
  saveButtonText: string;
  cancelButtonIcon: ReactNode
  backgroundButtonText: string;
  onSave: (title: string, background: string) => void
  onCancel: () => void
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

const base64ToBlob = (base64: string): Blob => {
  return new Blob([atob(base64)], {type: "image/svg+xml"})
}

const LanEditor = (params: LanEditorParams) => {
  let {alertError} = useAlerts()
  let [title, setTitle] = useState<string | null>((params.lan !== undefined) ? params.lan.title : null)
  let [background, setBackground] = useState<Blob | null>((params.lan !== undefined) ? base64ToBlob(params.lan.background) : null)
  let [backgroundName, setBackgroundName] = useState<string | null>(null)
  let [viewBackground, setViewBackground] = useState<boolean>(false)
  let [errors, setErrors] = useState<{ title: boolean, background: boolean }>({title: false, background: false})

  const onCancelPressed = () => {
    params.onCancel()
  }

  const onSavePressed = async () => {
    setErrors({
      title: (title == null || title.length == 0),
      background: (background == null || background.length == 0)
    })

    if (title != null && background != null) {
      let base64 = await blobToBase64(background)

      params.onSave(title, base64)
    }
  }

  const onTitleChanged = (event: React.ChangeEvent<HTMLInputElement>) => {
    let value = event.target.value
    if (value.length > 0) {
      setTitle(value)
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
          setBackground(new Blob([bytes], {type: "image/svg+xml"}))
          setBackgroundName(file.name)
        } else {
          alertError("Kunne ikke lese filen")
        }
      };

      reader.onerror = function (e) {
        alertError("Kunne ikke lese filen")
      }

      reader.readAsArrayBuffer(file)
    }
  }

  return (
    <Stack spacing={2} justifyContent="center" alignItems="center" sx={{width: "100%"}}>
      <TextField defaultValue={title} sx={{width: "100%"}} required label="Lan name" variant="outlined"
                 onChange={onTitleChanged} error={errors.title}/>
      <Button sx={{width: "100%"}} color={errors.background && "error" || "primary"} component="label"
              variant="outlined" startIcon={<UploadIcon/>}>
        <Typography noWrap={true}>{backgroundName || params.backgroundButtonText}</Typography>
        <VisuallyHiddenInput type="file" onInput={onBackgroundChanged} accept="image/svg"/>
      </Button>
      {!viewBackground && background &&
          <Button onClick={() => setViewBackground(true)} startIcon={<FullscreenIcon/>}>View background</Button>}
      {viewBackground && background &&
          <Button onClick={() => setViewBackground(false)} startIcon={<FullscreenExitIcon/>}>Hide background</Button>}
      {viewBackground && background &&
          <img alt="preview" style={{width: "100%"}} src={URL.createObjectURL(background)}/>}
      <Stack sx={{width: "100%"}} spacing={1} direction="row" justifyContent="space-between" alignItems="center">
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

export default function Admin() {
  let {alertSuccess, alertLoading} = useAlerts()
  let {showDialog} = useDialogs()
  let {lans, isLoading, reloadLans, createNewLan, deleteLan, updateLan} = useLans()
  let [isCreateVisible, setCreateVisible] = useState<boolean>(false);
  let [expanded, setExpanded] = useState<string | false>(false);

  const handleLanPressed = (lan: string) => (event: React.SyntheticEvent, newExpanded: boolean) => {
    setExpanded(newExpanded ? lan : false);
  };

  const onCreate = async (title: string, background: string) => {
    await alertLoading("Oppretter " + title, async () => {
      await createNewLan(title, background)
      await reloadLans()
    })
    setCreateVisible(false)
    alertSuccess(title + " har blitt opprettet")
  }

  const onUpdate = async (lan: Lan, title: string, background: string) => {
    await alertLoading("Oppdaterer " + title, async () => {
      await updateLan(lan, title, background)
      await reloadLans()
    })
    setExpanded(false)
    alertSuccess(title + " har blitt oppdatert")
  }

  const onCancel = () => {
    setCreateVisible(false)
  }

  const onDelete = async (lan: Lan) => {
    showDialog<Lan>({
      title: "Er du sikker?",
      description: `Er du sikker på at du vil slette lanet \"${lan.title}\"? Denne operasjonen kan ikke angres.`,
      positiveText: "Ja",
      negativeText: "Nei",
      positiveCallback: async (lan: Lan) => {
        await alertLoading("Sletter " + lan.title, async () => {
          await deleteLan(lan)
          await reloadLans()
        })

        alertSuccess(lan.title + " har blitt slettet")
      },
      metadata: lan,
    })
  } 

  return (
    <Stack divider={<Divider orientation="horizontal" flexItem/>} spacing={2} justifyContent="center"
           alignItems="center" sx={{marginTop: "1em"}}>
      <Stack direction="row" width="100%" justifyContent="space-between">
        <Typography variant="h4">Alle lan</Typography>
        <Button onClick={() => setCreateVisible(true)} startIcon={<Add/>} variant="outlined" color="secondary">Nytt
          lan</Button>
      </Stack>
      {isCreateVisible && <LanEditor backgroundButtonText={"Upload background"} saveButtonText={"Create"}
                                     cancelButtonIcon={<CancelIcon/>}
                                     onSave={(title, background) => onCreate(title, background)} onCancel={onCancel}/>}
      <Stack justifyContent="center" alignItems="center" sx={{marginTop: "1em"}} width="100%">
        {lans.map(lan => (
          <Accordion expanded={expanded == lan.id} onChange={handleLanPressed(lan.id)} sx={{width: "100%"}}>
            <AccordionSummary sx={{width: "100%"}}>
              <Stack width="100%" direction="row" spacing={2} justifyContent="space-between">
                <Typography>{lan.title}</Typography>
                <Typography color="textSecondary" variant="subtitle2">{lan.createdAt.toLocaleDateString("no", {day: "2-digit", month: "2-digit", year: "numeric"})}</Typography>
              </Stack>
            </AccordionSummary>
            <Divider orientation="horizontal" flexItem/>
            <AccordionDetails sx={{ paddingTop: "1em" }}>
              <LanEditor lan={lan} backgroundButtonText={"Change background"} saveButtonText={"Save changes"}
                         cancelButtonIcon={<DeleteIcon/>}
                         onSave={(title, background) => onUpdate(lan, title, background)}
                         onCancel={() => onDelete(lan)}/>
            </AccordionDetails>
          </Accordion>
        ))}
        {lans.length == 0 && isLoading && <DelayedCircularProgress/>}
      </Stack>
    </Stack>
  )
}
