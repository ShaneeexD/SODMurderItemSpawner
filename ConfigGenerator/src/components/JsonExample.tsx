import React from 'react';
import { Box, Typography, Paper } from '@mui/material';
import SyntaxHighlighter from 'react-syntax-highlighter';
import { atomOneDark } from 'react-syntax-highlighter/dist/esm/styles/hljs';

interface JsonExampleProps {
  json: any;
  title?: string;
}

const JsonExample: React.FC<JsonExampleProps> = ({ json, title }) => {
  const jsonString = JSON.stringify(json, null, 2);

  return (
    <Paper elevation={2} sx={{ p: 2, mb: 2 }}>
      {title && <Typography variant="h6" gutterBottom>{title}</Typography>}
      <Box sx={{ overflow: 'auto', maxHeight: '400px' }}>
        <SyntaxHighlighter language="json" style={atomOneDark}>
          {jsonString}
        </SyntaxHighlighter>
      </Box>
    </Paper>
  );
};

export default JsonExample;