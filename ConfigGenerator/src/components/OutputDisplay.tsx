import React from 'react';
import { 
  Paper, 
  Typography, 
  Button, 
  Stack,
  Box
} from '@mui/material';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { vscDarkPlus } from 'react-syntax-highlighter/dist/esm/styles/prism';
import type { SpawnRule } from '../models/configTypes';

interface OutputDisplayProps {
  rule: SpawnRule;
}

const OutputDisplay: React.FC<OutputDisplayProps> = ({ rule }) => {
  // Create a clean version of the rule for JSON output
  const cleanRule = { ...rule };
  
  // Remove undefined or empty optional properties
  Object.keys(cleanRule).forEach(key => {
    const typedKey = key as keyof SpawnRule;
    if (cleanRule[typedKey] === undefined || 
        (Array.isArray(cleanRule[typedKey]) && (cleanRule[typedKey] as any).length === 0) ||
        cleanRule[typedKey] === '') {
      delete cleanRule[typedKey];
    }
  });

  // Format the rule as a JSON string with proper indentation
  const jsonOutput = JSON.stringify({ SpawnRules: [cleanRule] }, null, 2);

  const handleCopyToClipboard = () => {
    navigator.clipboard.writeText(jsonOutput);
  };

  const handleDownload = () => {
    const blob = new Blob([jsonOutput], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    
    // Use the rule name and append 'MIS' to the filename
    const fileName = rule.Name ? 
      `${rule.Name.replace(/[\/:*?"<>|]/g, '_')}MIS.json` : 
      'spawn-ruleMIS.json';
    
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  };

  return (
    <Paper elevation={3} sx={{ p: 2 }}>
      <Stack spacing={2}>
        <Typography variant="h6">
          Generated JSON Configuration
        </Typography>
        
        <Box>
          <SyntaxHighlighter language="json" style={vscDarkPlus} wrapLongLines>
            {jsonOutput}
          </SyntaxHighlighter>
        </Box>
        
        <Box sx={{ display: 'flex', gap: 2 }}>
          <Box sx={{ flex: 1 }}>
            <Button 
              variant="contained" 
              color="primary" 
              fullWidth
              onClick={handleCopyToClipboard}
            >
              Copy to Clipboard
            </Button>
          </Box>
          <Box sx={{ flex: 1 }}>
            <Button 
              variant="outlined" 
              color="primary" 
              fullWidth
              onClick={handleDownload}
            >
              Download JSON
            </Button>
          </Box>
        </Box>
      </Stack>
    </Paper>
  );
};

export default OutputDisplay;
