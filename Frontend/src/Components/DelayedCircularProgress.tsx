import * as React from 'react';
import Box from '@mui/material/Box';
import Fade from '@mui/material/Fade';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Typography from '@mui/material/Typography';
import {CircularProgressProps} from "@mui/material/CircularProgress/CircularProgress";
import Container from "@mui/material/Container";
import {useEffect, useState} from "react";

export default function DelayedCircularProgress(params: CircularProgressProps) {
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    setTimeout(() => {
      setVisible(true)
    }, 1000)
  }, []);

  return visible ? <CircularProgress/> : <Container/>
}