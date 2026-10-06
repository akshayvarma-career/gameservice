param(
    [Parameter(Mandatory)] [string] $InFile,
    [Parameter(Mandatory)] [string] $OutFile
)
# Converts a docs-tool JSON read (prose XML) into Markdown.

$raw = [IO.File]::ReadAllText($InFile)
$xml = ($raw | ConvertFrom-Json).data.xml
$doc = New-Object System.Xml.XmlDocument
$doc.LoadXml($xml)

function Inline($node) {
    $sb = ''
    foreach ($c in $node.ChildNodes) {
        switch ($c.Name) {
            '#text'   { $sb += $c.Value }
            'text'    { $sb += (Inline $c) }
            'bold'    { $sb += '**' + (Inline $c) + '**' }
            'italic'  { $sb += '*' + (Inline $c) + '*' }
            'code'    { $sb += '`' + (Inline $c) + '`' }
            'link'    { $sb += '[' + (Inline $c) + '](' + $c.GetAttribute('href') + ')' }
            'date'    { $sb += $c.GetAttribute('value') }
            'mention' { $sb += $c.GetAttribute('name') }
            'break'   { $sb += ' ' }
            default   { $sb += (Inline $c) }
        }
    }
    $sb
}

function Block($node, $indent) {
    $out = @()
    foreach ($c in $node.ChildNodes) {
        switch ($c.Name) {
            'paragraph' {
                $t = Inline $c
                $h = $c.GetAttribute('heading')
                if ($h) { $out += ''; $out += ('#' * [int]$h) + ' ' + $t; $out += '' }
                else { $out += $indent + $t; if (-not $indent) { $out += '' } }
            }
            'list' {
                $kind = $c.GetAttribute('kind'); $i = 1
                foreach ($li in $c.ChildNodes) {
                    if ($li.Name -ne 'listItem') { continue }
                    $marker = switch ($kind) {
                        'ordered' { "$i." }
                        'check'   { if ($li.GetAttribute('checked') -eq 'true') { '- [x]' } else { '- [ ]' } }
                        default   { '-' }
                    }
                    $i++
                    $first = $true
                    foreach ($p in $li.ChildNodes) {
                        if ($p.Name -eq 'paragraph') {
                            if ($first) { $out += "$indent$marker " + (Inline $p); $first = $false }
                            else { $out += "$indent    " + (Inline $p) }
                        } elseif ($p.Name -eq 'list') {
                            $wrap = $doc.CreateElement('w'); $wrap.AppendChild($p.CloneNode($true)) | Out-Null
                            $out += Block $wrap ($indent + '    ')
                        }
                    }
                }
                if (-not $indent) { $out += '' }
            }
            'table' {
                $r = 0
                foreach ($row in $c.ChildNodes) {
                    if ($row.Name -ne 'row') { continue }
                    $cells = @()
                    foreach ($cell in $row.ChildNodes) {
                        if ($cell.Name -ne 'cell') { continue }
                        $cells += ((@($cell.ChildNodes | ForEach-Object { Inline $_ }) -join ' ') -replace '\|', '\|')
                    }
                    $out += '| ' + ($cells -join ' | ') + ' |'
                    if ($r -eq 0) { $out += '|' + (' --- |' * $cells.Count) }
                    $r++
                }
                $out += ''
            }
            default { $out += Block $c $indent }
        }
    }
    $out
}

$lines = Block $doc.DocumentElement ''
$md = ($lines -join "`n") -replace "(`n){3,}", "`n`n"
[IO.File]::WriteAllText($OutFile, $md.Trim() + "`n", (New-Object System.Text.UTF8Encoding($false)))
"Wrote $OutFile ($($md.Length) chars)"
