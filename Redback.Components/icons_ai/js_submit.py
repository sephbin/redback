"""Print a JS snippet that types the prompt for <name> into Gemini and sends it (run after navigating to /app)."""
import json, sys, os
P = json.load(open(os.path.join(os.path.dirname(__file__), 'prompts.json'), encoding='utf-8'))
print("const P=%s; let ed=null; for(let i=0;i<40&&!ed;i++){ed=document.querySelector('[aria-label=\"Enter a prompt for Gemini\"]'); if(!ed) await new Promise(r=>setTimeout(r,250));} "
      "ed.focus(); document.execCommand('insertText', false, P); await new Promise(r=>setTimeout(r,800)); "
      "const b=[...document.querySelectorAll('button[aria-label]')].find(x=>/send/i.test(x.getAttribute('aria-label'))); "
      "b? (b.click(),'sent'):'no send button'" % json.dumps(P[sys.argv[1]]))
