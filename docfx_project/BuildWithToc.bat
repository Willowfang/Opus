:: Create metadata for modification
docfx metadata
:: Reshape TOC with namespace nesting
node namespaces.js
:: Build site
docfx build